FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app
COPY . ./
RUN dotnet publish -c Release -o output

FROM nginx:alpine
WORKDIR /var/www/web

# Install OpenSSL
RUN apk add --no-cache openssl

RUN mkdir /etc/nginx/ssl
COPY Client/nginx/generate_ssl.sh /usr/local/bin/generate_ssl.sh
RUN chmod +x /usr/local/bin/generate_ssl.sh
RUN /usr/local/bin/generate_ssl.sh

COPY --from=build-env /app/output/wwwroot .
COPY Client/nginx/nginx.conf /etc/nginx/nginx.conf

EXPOSE 80
EXPOSE 443