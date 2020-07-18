#!/bin/bash

# Generate private key
openssl genrsa -out client.key 4096

# Generate certificate signing request (csr)
openssl req -sha256 -new -key client.key -out client.csr

# Generate a certificate from the csr
openssl x509 -req -sha256 -days 365 -in client.csr -signkey client.key -out client.cer

# Extract the public key
openssl rsa -in client.key -pubout > client.pub

# Export the certificate in pkcs12 format
openssl pkcs12 -export -out client.pfx -inkey client.key -in client.cer

# Import client.pfx into the Personal store
# Import client.cer into the Trusted Root store

# generating development certificate for kestrel
openssl req -new -x509 -newkey rsa:2048 -keyout dev-certificate.key -out dev-certificate.cer -days 365 -subj "/CN=localhost/O=CloudAuthority/C=AU"
openssl pkcs12 -export -out dev-certificate.pfx -inkey dev-certificate.key -in dev-certificate.cer

