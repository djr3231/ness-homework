import {
  getApiOperations,
  postApiCalculations,
  type ExecuteOperationRequest,
} from "./generated";
import { client } from "./generated/client.gen";

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;

if (!apiBaseUrl) {
  throw new Error("VITE_API_BASE_URL is required");
}

client.setConfig({
  baseUrl: apiBaseUrl,
});

export function loadOperations() {
  return getApiOperations();
}

export function calculate(request: ExecuteOperationRequest) {
  return postApiCalculations({
    body: request,
  });
}

export type { OperationResponse } from "./generated";
