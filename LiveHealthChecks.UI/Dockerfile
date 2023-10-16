FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build-env
RUN apk add nodejs
RUN apk add npm

WORKDIR /app
COPY . ./

RUN dotnet publish "LiveHealthChecks.UI/LiveHealthChecks.UI.csproj" -c Release -o output

FROM nginx:alpine
WORKDIR /user/share/nginx/html
COPY --from=build-env /app/output/wwwroot .
COPY LiveHealthChecks.UI/nginx.conf /etc/nginx/nginx.conf
COPY LiveHealthChecks.UI/Certificates/livehealthchecks.ui.crt /etc/nginx/livehealthchecks.ui.crt
COPY LiveHealthChecks.UI/Certificates/livehealthchecks.ui.key /etc/nginx/livehealthchecks.ui.key
COPY LiveHealthChecks.UI/Certificates/livehealthchecks.ui.pem /etc/nginx/livehealthchecks.ui.pem
RUN chmod 754 /etc/nginx/livehealthchecks.ui.crt
RUN chmod 754 /etc/nginx/livehealthchecks.ui.key
RUN chmod 754 /etc/nginx/livehealthchecks.ui.pem

EXPOSE 80
EXPOSE 443