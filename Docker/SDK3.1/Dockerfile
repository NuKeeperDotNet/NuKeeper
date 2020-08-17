FROM mcr.microsoft.com/dotnet/core/sdk:3.1-bionic
ARG NUKEEPER_VERSION=0.33.0
RUN dotnet tool install --global NuKeeper --version $NUKEEPER_VERSION
ENV PATH="${PATH}:/root/.dotnet/tools"
ENTRYPOINT ["nukeeper"]
