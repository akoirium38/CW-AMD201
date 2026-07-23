import {BrowserRouter, Route, Routes} from "react-router";
import {Toaster} from "sonner";
import AuthPage from "./pages/AuthPage";

function App() {

  return (
    <>
      <BrowserRouter>
        <Routes>
          {/* Public Routes */}
          <Route path="/auth" element={<AuthPage />} />


          {/* Protected Routes */}
        </Routes>
      </BrowserRouter>
    </>
  )
}

export default App
