import {
  useEffect,
  useState,
  type SubmitEvent as ReactSubmitEvent,
} from "react";
import {
  calculate,
  loadOperations,
  type OperationResponse,
} from "../api/client";

export function OperationCalculator() {
  const [isLoading, setLoading] = useState<boolean>(true);
  const [operations, setOperations] = useState<OperationResponse[]>([]);
  const [selectedOperation, setSelectedOperation] = useState<string>("");
  const [loadError, setLoadError] = useState<string | null>(null);
  const [left, setLeft] = useState<string>("");
  const [right, setRight] = useState<string>("");
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);
  const [result, setResult] = useState<string | null>(null);
  const [calculationError, setCalculationError] = useState<string | null>(null);

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

  const handleSubmit = async (e: ReactSubmitEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (isSubmitting || selectedOperation === "") {
      return;
    }
    setResult(null);
    setCalculationError(null);
    setIsSubmitting(true);
    try {
      const { data, error } = await calculate({
        operation: selectedOperation,
        left,
        right,
      });
      if (error) {
        setCalculationError(
          error.detail ?? error.title ?? "Unable to calculate.",
        );
        return;
      }
      if (!data) {
        setCalculationError("Unable to calculate.");
        return;
      }

      setResult(data.result);
    } catch {
      setCalculationError("Unable to calculate.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div
      id="wrapper"
      className="flex justify-center h-screen text-blue-100 bg-blue-950"
    >
      <div className="flex flex-col gap-4 justify-center">
        <section aria-labelledby="calculator-title">
          <h1 id="calculator-title" className="text-center text-2xl font-bold">
            Project Gaia
          </h1>

          <div className="flex gap-2 p-5 justify-between items-center bg-blue-900 rounded-md">
            <form className="flex gap-x-4 items-center" onSubmit={handleSubmit}>
              <div className="flex flex-col items-center p-1 rounded-md bg-blue-700">
                <label htmlFor="field-a">Field A</label>
                <input
                  id="field-a"
                  value={left}
                  disabled={isSubmitting}
                  type="text"
                  className="bg-blue-800 rounded-md p-2 focus:outline-2 focus:outline-blue-900"
                  onChange={(e) => setLeft(e.target.value)}
                ></input>
              </div>

              <div className="flex flex-col items-center p-1 rounded-md bg-blue-700">
                <label htmlFor="operation">Operation</label>

                <select
                  id="operation"
                  value={selectedOperation}
                  disabled={
                    isLoading ||
                    isSubmitting ||
                    loadError !== null ||
                    operations.length === 0
                  }
                  aria-describedby={
                    statusMessage ? "operation-status" : undefined
                  }
                  onChange={(event) => {
                    setCalculationError(null);
                    setSelectedOperation(event.currentTarget.value);
                  }}
                  className="bg-blue-800 p-2 rounded-md text-center cursor-pointer focus:outline-2 focus:outline-blue-900"
                >
                  {operations.map(({ key, displayName }) => (
                    <option className="bg-blue-950 " key={key} value={key}>
                      {displayName}
                    </option>
                  ))}
                </select>
              </div>

              <div className="flex flex-col items-center p-1 rounded-md bg-blue-700">
                <label htmlFor="field-b">Field B</label>
                <input
                  id="field-b"
                  value={right}
                  disabled={isSubmitting}
                  type="text"
                  className="bg-blue-800 rounded-md p-2 focus:outline-2 focus:outline-blue-900"
                  onChange={(e) => setRight(e.target.value)}
                ></input>
              </div>
              <button
                disabled={isSubmitting || statusMessage !== null}
                className="p-2 bg-blue-600 rounded-md font-bold text-lg cursor-pointer"
              >
                {isSubmitting ? "Calculating..." : "Calculate"}
              </button>
            </form>
          </div>
        </section>
        {result !== null && (
          <p
            role="status"
            className="text-lg text-center font-bold p-3 rounded-lg bg-emerald-300 text-emerald-900"
          >
            Result: {result}
          </p>
        )}
        {calculationError && (
          <p
            role="alert"
            className="text-lg font-bold p-3 rounded-lg bg-red-300 text-red-900"
          >
            {calculationError}
          </p>
        )}
        {statusMessage && (
          <div
            className={`p-3 rounded-xl font-bold ${
              loadError
                ? "bg-red-200 text-red-800"
                : "bg-blue-200 text-blue-950"
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
