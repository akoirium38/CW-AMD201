import {BrowserRouter, Route, Routes} from "react-router";
import {Toaster} from "sonner";
import AuthPage from "./pages/AuthPage";
import ProtectedRoute from "./components/auth/ProtectedRoute";
import Home from "./pages/Home";

function App() {

  return (
    <>
      <BrowserRouter>
        <Routes>
          {/* Public Routes */}
          <Route path="/auth" element={<AuthPage />} />


          {/* Protected Routes */}

          <Route element={<ProtectedRoute />}>
            <Route path="/" element={<Home/>} />
          </Route>
        </Routes>
      </BrowserRouter>
    </>
  )
}

export default App
