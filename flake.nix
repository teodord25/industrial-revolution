{
  description = "A Nix-flake-based C# development environment";

  inputs.nixpkgs.url = "https://flakehub.com/f/NixOS/nixpkgs/0.1.*.tar.gz";

  outputs =
    { self, nixpkgs }:
    let
      supportedSystems = [
        "x86_64-linux"
        "aarch64-linux"
        "x86_64-darwin"
        "aarch64-darwin"
      ];
      forEachSupportedSystem =
        f:
        nixpkgs.lib.genAttrs supportedSystems (
          system:
          f {
            pkgs = import nixpkgs { inherit system; };
          }
        );

      # Import local config with fallback
      localConfig =
        if builtins.pathExists ./local.nix then import ./local.nix else { vintageStoryPath = ""; };
    in
    {
      devShells = forEachSupportedSystem (
        { pkgs }:
        {
          default = pkgs.mkShell {
            packages = with pkgs; [
              #dotnet-sdk_6
              #dotnet-sdk_7
              dotnet-sdk_8
              omnisharp-roslyn
              mono
              msbuild

              # for prototyping
              python3
              inotify-tools
            ];
            shellHook = ''
              # Source .env if it exists
              if [ -f .env ]; then
                source .env
              fi

              if [ -z "$VINTAGE_STORY" ]; then
                echo " WARNING: VINTAGE_STORY not set!"
                echo "Create a .env file with:"
                echo "  export VINTAGE_STORY=/path/to/VintageStory/installation"
              fi

              alias bld="./build.sh"
              alias dbg="./debug.sh"
              alias prt="./prototype.sh"
            '';
          };
        }
      );
    };
}
