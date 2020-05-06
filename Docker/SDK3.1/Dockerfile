FROM mcr.microsoft.com/dotnet/core/sdk:3.1-bionic
RUN dotnet tool install --global NuKeeper --version 0.28.0
ENV PATH="${PATH}:/root/.dotnet/tools"
ENTRYPOINT ["nukeeper"]
