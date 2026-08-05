import { defineConfig } from "@hey-api/openapi-ts";

export default defineConfig({
  input: "../server/Gaia.Api/openapi/gaia-api.json",
  output: "src/api/generated",
  plugins: ["@hey-api/typescript", "@hey-api/sdk", "@hey-api/client-fetch"],
});
