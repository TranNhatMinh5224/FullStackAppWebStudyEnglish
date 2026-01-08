import { BrowserRouter } from "react-router-dom";
import AppRoutes from "./Routes";
import { EnumProvider } from "./Context/EnumContext";
import { ToastContainer } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";

function App() {
  return (
    <BrowserRouter>
      <EnumProvider>
        <AppRoutes />
        <ToastContainer position="top-right" autoClose={3000} />
      </EnumProvider>
    </BrowserRouter>
  );
}

export default App;
