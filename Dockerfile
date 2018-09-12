FROM microsoft/dotnet:2.1-sdk as builder

COPY . /build
WORKDIR /build

RUN dotnet test NuKeeper.Tests/NuKeeper.Tests.csproj --filter "TestCategory!=WindowsOnly"
RUN dotnet test NuKeeper.Inspection.Tests/NuKeeper.Inspection.Tests.csproj --filter "TestCategory!=WindowsOnly"
RUN dotnet test NuKeeper.Update.Tests/NuKeeper.Update.Tests.csproj --filter "TestCategory!=WindowsOnly"
RUN dotnet test NuKeeper.Integration.Tests/NuKeeper.Integration.Tests.csproj --filter "TestCategory!=WindowsOnly"

RUN dotnet publish -c Release ./NuKeeper/NuKeeper.csproj -f netcoreapp2.1

FROM microsoft/dotnet:2.1-sdk

COPY --from=builder /build/NuKeeper/bin/Release/netcoreapp2.1/publish/ /app/

WORKDIR /app

RUN echo "[user]" > /root/.gitconfig \
  && echo "  name = GOCD NuKeeper" >> /root/.gitconfig \
  && echo "  email = gocd.nukeeper@noreply.com" >> /root/.gitconfig

ENTRYPOINT ["dotnet", "NuKeeper.dll"]
