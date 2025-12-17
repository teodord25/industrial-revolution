SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
dotnet run --project $SCRIPT_DIR/CakeBuild/CakeBuild.csproj -- "$@"
