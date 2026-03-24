{pkgs ? import <nixpkgs> {}}:
pkgs.mkShell {
  packages = with pkgs; [
    go
    wgo
    (go-migrate.overrideAttrs (_: {
      tags = ["postgres" "file"];
    }))
    sqlc
    go-swag
    # for psql in cli (+ pgformatter for sql files)
    postgresql_18
    pgformatter
    # to run tests
    hurl
  ];
}
