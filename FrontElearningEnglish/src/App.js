import { BrowserRouter } from "react-router-dom";
import AppRoutes from "./Routes";
import { EnumProvider } from "./Context/EnumContext";
import { ThemeProvider } from "./Context/ThemeContext";
import { AssetProvider } from "./Context/AssetContext";
import { ToastContainer } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import "./Components/Common/ModalFix/ModalFix.css";
import "./Components/Common/Modal/BaseModal.css";

function App() {
  return (
    <BrowserRouter>
      <ThemeProvider>
        <EnumProvider>
          <AssetProvider>
            <AppRoutes />
            <ToastContainer position="top-right" autoClose={3000} />
          </AssetProvider>
        </EnumProvider>
      </ThemeProvider>
    </BrowserRouter>
  );
}

export default App;
