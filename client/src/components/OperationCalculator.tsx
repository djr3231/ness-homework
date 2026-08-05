import { useEffect, useState } from "react";
import { loadOperations, type OperationResponse } from "../api/client";

export function OperationCalculator() {
  const [isLoading, setLoading] = useState<boolean>(true);
  const [operations, setOperations] = useState<OperationResponse[]>([]);
  const [selectedOperation, setSelectedOperation] = useState<string>("");
  const [loadError, setLoadError] = useState<string | null>(null);

  useEffect(() => {
    let ignore = false;

    const fetchOperations = async () => {
      try {
        const { data, error } = await loadOperations();
        if (ignore) {
          return;
        }

        if (error || !data) {
          setLoadError("Unable to load operations.");
          return;
        }

        setOperations(data);
        setSelectedOperation(data[0]?.key ?? "");
      } catch {
        if (!ignore) {
          setLoadError("Unable to load operations.");
        }
      } finally {
        if (!ignore) {
          setLoading(false);
        }
      }
    };

    void fetchOperations();

    return () => {
      ignore = true;
    };
  }, []);

  const statusMessage = isLoading
    ? "Loading operations..."
    : loadError
      ? loadError
      : operations.length === 0
        ? "No operations available."
        : null;

  return (
    <div id="wrapper" className="flex justify-center ">
      <div className="flex flex-col gap-4">
        <section aria-labelledby="calculator-title">
          <h1 id="calculator-title" className="text-center text-2xl font-bold">
            Project Gaia
          </h1>

          <div className="flex gap-2 p-2 justify-between items-center bg-blue-400 rounded-md">
            <div className="flex flex-col items-center">
              <label htmlFor="operation">Operation</label>

              <select
                id="operation"
                value={selectedOperation}
                disabled={
                  isLoading || loadError !== null || operations.length === 0
                }
                aria-describedby={
                  statusMessage ? "operation-status" : undefined
                }
                onChange={(event) =>
                  setSelectedOperation(event.currentTarget.value)
                }
              >
                {operations.map(({ key, displayName }) => (
                  <option key={key} value={key}>
                    {displayName}
                  </option>
                ))}
              </select>
            </div>
          </div>
        </section>
        {statusMessage && (
          <div
            className={`p-3 rounded-xl font-bold ${
              loadError ? "bg-red-200" : "bg-blue-200"
            }`}
          >
            <p id="operation-status" role={loadError ? "alert" : "status"}>
              {statusMessage}
            </p>
          </div>
        )}
      </div>
    </div>
  );
}
