FROM --platform=linux/arm64/v8 mcr.microsoft.com/dotnet/runtime:6.0

ADD build/output /opt/synthetics

VOLUME ["/data"]

WORKDIR /data

ENTRYPOINT ["/opt/synthetics/Ae.Synthetics.Console"]
