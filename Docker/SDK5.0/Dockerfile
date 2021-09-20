FROM mcr.microsoft.com/dotnet/sdk:5.0-focal
ARG NUKEEPER_VERSION=0.34.0
RUN dotnet tool install --global NuKeeper --version $NUKEEPER_VERSION
ENV PATH="${PATH}:/root/.dotnet/tools"
ENTRYPOINT ["nukeeper"]
