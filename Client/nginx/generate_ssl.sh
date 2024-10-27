#!/bin/sh

# Directory for SSL certificates
SSL_DIR="/etc/nginx/ssl"
ROOT_CA_KEY="$SSL_DIR/rootCA.key"
ROOT_CA_CERT="$SSL_DIR/rootCA.pem"
ROOT_CA_SERIAL="$SSL_DIR/rootCA.srl"
LOCALHOST_KEY="$SSL_DIR/key.pem"
LOCALHOST_CERT="$SSL_DIR/cert.pem"
DAYS_VALID=365

# Ensure SSL directory exists
mkdir -p $SSL_DIR

# Generate SSL certificates if they don't exist
if [ ! -f "$LOCALHOST_CERT" ] || [ ! -f "$LOCALHOST_KEY" ]; then
  echo "Generating Root CA key and certificate..."

  # Step 1: Generate Root CA Key and Certificate
  openssl genrsa -out $ROOT_CA_KEY 2048
  openssl req -x509 -new -nodes -key $ROOT_CA_KEY -sha256 -days 1024 -out $ROOT_CA_CERT -subj "/C=US/ST=Local/L=Local/O=Local CA/OU=Local CA/CN=Local CA"

  # Step 2: Generate Localhost Key and CSR
  echo "Generating localhost private key and CSR..."
  openssl req -new -newkey rsa:2048 -nodes -keyout $LOCALHOST_KEY -out "$SSL_DIR/localhost.csr" -subj "/C=US/ST=Local/L=Local/O=Local Development/OU=IT/CN=localhost"

  # Step 3: Create Extensions File for Localhost Certificate
  cat > "$SSL_DIR/v3.ext" <<- EOF
authorityKeyIdentifier=keyid,issuer
basicConstraints=CA:FALSE
keyUsage = digitalSignature, nonRepudiation, keyEncipherment, dataEncipherment
subjectAltName = @alt_names

[alt_names]
DNS.1 = localhost
EOF

  # Step 4: Generate and Sign Localhost Certificate with Root CA
  echo "Signing localhost certificate with Root CA..."
  openssl x509 -req -in "$SSL_DIR/localhost.csr" -CA $ROOT_CA_CERT -CAkey $ROOT_CA_KEY -CAcreateserial -out $LOCALHOST_CERT -days $DAYS_VALID -sha256 -extfile "$SSL_DIR/v3.ext"

  # Cleanup the temporary CSR and extensions file
  rm "$SSL_DIR/localhost.csr" "$SSL_DIR/v3.ext"

  echo "SSL certificates generated and stored in $SSL_DIR."
  echo "To trust the Root CA, add $ROOT_CA_CERT to your system's Trusted Root Certification Authorities."
else
  echo "SSL certificates already exist in $SSL_DIR. Skipping generation."
fi