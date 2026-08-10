#!/usr/bin/env bash
# One-time Azure provisioning for SwiftBite's 8 backend services on Container Apps.
# Run this yourself, once, after `az login`. Safe to re-run individual sections if something fails partway.
set -euo pipefail

# ── Fill these in before running ────────────────────────────────────────────
RESOURCE_GROUP="swiftbite-rg"
LOCATION="eastus"                     # pick a region close to you
GITHUB_USERNAME="CHANGE_ME"           # your GitHub username (for ghcr.io image paths)
AZURE_SQL_CONNECTION="CHANGE_ME"      # full Azure SQL connection string from portal
REDIS_CONNECTION="CHANGE_ME"          # Upstash: <host>:6379,password=<pw>,ssl=True,abortConnect=False
KAFKA_BOOTSTRAP_SERVERS="CHANGE_ME"   # Upstash Kafka bootstrap endpoint
KAFKA_SASL_USERNAME="CHANGE_ME"
KAFKA_SASL_PASSWORD="CHANGE_ME"
RAZORPAY_KEY_ID="CHANGE_ME"
RAZORPAY_KEY_SECRET="CHANGE_ME"
RAZORPAY_WEBHOOK_SECRET="CHANGE_ME"
GOOGLE_CLIENT_ID="CHANGE_ME"
GOOGLE_CLIENT_SECRET="CHANGE_ME"
FRONTEND_URL="https://CHANGE_ME.vercel.app"   # update once you know it; re-run the "update CORS" section after

# 7 random secrets shared between AuthServer and each downstream service - generate once, reuse everywhere
OIDC_GATEWAY_SECRET=$(openssl rand -hex 32)
OIDC_USERSERVICE_SECRET=$(openssl rand -hex 32)
OIDC_RESTAURANTSERVICE_SECRET=$(openssl rand -hex 32)
OIDC_ORDERSERVICE_SECRET=$(openssl rand -hex 32)
OIDC_PAYMENTSERVICE_SECRET=$(openssl rand -hex 32)
OIDC_NOTIFICATIONSERVICE_SECRET=$(openssl rand -hex 32)
OIDC_DELIVERYSERVICE_SECRET=$(openssl rand -hex 32)

PLACEHOLDER_IMAGE="mcr.microsoft.com/k8se/quickstart:latest"  # swapped for the real image on first CI deploy
# ─────────────────────────────────────────────────────────────────────────────

echo "==> Resource group + Container Apps environment"
az group create --name "$RESOURCE_GROUP" --location "$LOCATION"
az extension add --name containerapp --upgrade -y
az provider register --namespace Microsoft.App
az provider register --namespace Microsoft.OperationalInsights
az containerapp env create --name swiftbite-env --resource-group "$RESOURCE_GROUP" --location "$LOCATION"

echo "==> AuthServer"
az containerapp create \
  --name swiftbite-authserver --resource-group "$RESOURCE_GROUP" --environment swiftbite-env \
  --image "$PLACEHOLDER_IMAGE" --target-port 8080 --ingress external \
  --env-vars \
    ASPNETCORE_ENVIRONMENT=Production \
    "ConnectionStrings__DefaultConnection=$AZURE_SQL_CONNECTION" \
    "AuthServer__AngularBaseUrl=$FRONTEND_URL" \
    "Cors__AllowedOrigins__0=$FRONTEND_URL" \
    "OpenIddictClients__GatewaySecret=$OIDC_GATEWAY_SECRET" \
    "OpenIddictClients__UserServiceSecret=$OIDC_USERSERVICE_SECRET" \
    "OpenIddictClients__RestaurantServiceSecret=$OIDC_RESTAURANTSERVICE_SECRET" \
    "OpenIddictClients__OrderServiceSecret=$OIDC_ORDERSERVICE_SECRET" \
    "OpenIddictClients__PaymentServiceSecret=$OIDC_PAYMENTSERVICE_SECRET" \
    "OpenIddictClients__NotificationServiceSecret=$OIDC_NOTIFICATIONSERVICE_SECRET" \
    "OpenIddictClients__DeliveryServiceSecret=$OIDC_DELIVERYSERVICE_SECRET" \
    "Google__ClientId=$GOOGLE_CLIENT_ID" \
    "Google__ClientSecret=$GOOGLE_CLIENT_SECRET"

AUTHSERVER_URL=$(az containerapp show --name swiftbite-authserver --resource-group "$RESOURCE_GROUP" --query properties.configuration.ingress.fqdn -o tsv)
echo "AuthServer live at: https://$AUTHSERVER_URL"
az containerapp update --name swiftbite-authserver --resource-group "$RESOURCE_GROUP" \
  --set-env-vars "AuthServer__Issuer=https://$AUTHSERVER_URL"

echo "==> ApiGateway"
az containerapp create \
  --name swiftbite-apigateway --resource-group "$RESOURCE_GROUP" --environment swiftbite-env \
  --image "$PLACEHOLDER_IMAGE" --target-port 8080 --ingress external \
  --env-vars \
    ASPNETCORE_ENVIRONMENT=Production \
    "AuthServer__Authority=https://$AUTHSERVER_URL" \
    "Redis__ConnectionString=$REDIS_CONNECTION" \
    "OpenIddictClients__GatewaySecret=$OIDC_GATEWAY_SECRET" \
    "Cors__AllowedOrigins__0=$FRONTEND_URL"

# service, db-conn-key, oidc-secret-key-name, oidc-secret-value, needs-kafka(yes/no)
declare -a SERVICES=(
  "userservice|UserServiceDb|UserServiceSecret|$OIDC_USERSERVICE_SECRET|no"
  "restaurantservice|RestaurantServiceDb|RestaurantServiceSecret|$OIDC_RESTAURANTSERVICE_SECRET|yes"
  "orderservice|OrderServiceDb|OrderServiceSecret|$OIDC_ORDERSERVICE_SECRET|yes"
  "paymentservice|PaymentServiceDb|PaymentServiceSecret|$OIDC_PAYMENTSERVICE_SECRET|yes"
  "deliveryservice|DeliveryServiceDb|DeliveryServiceSecret|$OIDC_DELIVERYSERVICE_SECRET|yes"
  "notificationservice|NotificationServiceDb|NotificationServiceSecret|$OIDC_NOTIFICATIONSERVICE_SECRET|yes"
)

for entry in "${SERVICES[@]}"; do
  IFS='|' read -r svc dbkey oidckey oidcsecret needskafka <<< "$entry"
  echo "==> $svc"
  ENV_VARS=(
    ASPNETCORE_ENVIRONMENT=Production
    "ConnectionStrings__DefaultConnection=$AZURE_SQL_CONNECTION"
    "ConnectionStrings__${dbkey}=$AZURE_SQL_CONNECTION"
    "AuthServer__Authority=https://$AUTHSERVER_URL"
    "Redis__ConnectionString=$REDIS_CONNECTION"
    "OpenIddictClients__${oidckey}=$oidcsecret"
    "Cors__AllowedOrigins__0=$FRONTEND_URL"
  )
  if [[ "$needskafka" == "yes" ]]; then
    ENV_VARS+=(
      "Kafka__BootstrapServers=$KAFKA_BOOTSTRAP_SERVERS"
      "Kafka__SaslUsername=$KAFKA_SASL_USERNAME"
      "Kafka__SaslPassword=$KAFKA_SASL_PASSWORD"
    )
  fi
  if [[ "$svc" == "paymentservice" ]]; then
    ENV_VARS+=(
      "Razorpay__KeyId=$RAZORPAY_KEY_ID"
      "Razorpay__KeySecret=$RAZORPAY_KEY_SECRET"
      "Razorpay__WebhookSecret=$RAZORPAY_WEBHOOK_SECRET"
    )
  fi
  az containerapp create \
    --name "swiftbite-$svc" --resource-group "$RESOURCE_GROUP" --environment swiftbite-env \
    --image "$PLACEHOLDER_IMAGE" --target-port 8080 --ingress external \
    --env-vars "${ENV_VARS[@]}"
done

echo "==> Service principal for GitHub Actions (save this JSON as the AZURE_CREDENTIALS secret)"
SUBSCRIPTION_ID=$(az account show --query id -o tsv)
az ad sp create-for-rbac \
  --name "swiftbite-github-actions" \
  --role contributor \
  --scopes "/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP" \
  --sdk-auth

echo ""
echo "==> Next steps:"
echo "1. Copy the JSON printed above into the GitHub secret AZURE_CREDENTIALS."
echo "2. Add a second GitHub secret AZURE_RESOURCE_GROUP with value: $RESOURCE_GROUP"
echo "3. Push to main - deploy.yml will build real images and replace the placeholder."
echo "4. Once your frontend is live at its real URL, re-run the Cors__AllowedOrigins__0"
echo "   and AuthServer__AngularBaseUrl / AuthServer__Issuer updates with the real values."
