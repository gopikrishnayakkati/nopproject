FROM alpine:3.18.2 AS extractor
ARG DOWNLOAD_URL="https://github.com/nopSolutions/nopCommerce/releases/download/release-4.60.3/nopCommerce_4.60.3_NoSource_linux_x64.zip"
ARG TARGET_FOLDER="/nop"
ADD ${DOWNLOAD_URL} ${TARGET_FOLDER}/nopCommerce_4.60.3_NoSource_linux_x64.zip
RUN apk update && \
    apk add unzip && \
    cd ${TARGET_FOLDER} && \
    unzip nopCommerce_4.60.3_NoSource_linux_x64.zip \
    && rm nopCommerce_4.60.3_NoSource_linux_x64.zip && \
    mkdir bin logs


FROM mcr.microsoft.com/dotnet/sdk:7.0
LABEL author=project
ARG TARGET_FOLDER="/nop"
ENV ASPNETCORE_URLS="http://0.0.0.0:5000"
COPY --from=extractor ${TARGET_FOLDER}  ${TARGET_FOLDER}
EXPOSE 5000
WORKDIR ${TARGET_FOLDER}
CMD ["dotnet", "Nop.Web.dll"]


