FROM microsoft/dotnet:2.1-sdk
RUN dotnet tool install --global NuKeeper --version 0.16.0
ENV PATH="${PATH}:/root/.dotnet/tools"
ENTRYPOINT ["nukeeper"]
