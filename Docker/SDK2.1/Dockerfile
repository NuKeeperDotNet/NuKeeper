FROM mcr.microsoft.com/dotnet/core/sdk:2.1
ARG NUKEEPER_VERSION=0.34.0
RUN dotnet tool install --global NuKeeper --version $NUKEEPER_VERSION
ENV PATH="${PATH}:/root/.dotnet/tools"
ENTRYPOINT ["nukeeper"]
