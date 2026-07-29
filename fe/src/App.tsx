import { BrowserRouter, Route, Routes } from "react-router";
import { Toaster } from "sonner";
import AuthPage from "./pages/AuthPage";
import ProtectedRoute from "./components/auth/ProtectedRoute";
import Home from "./pages/Home";
import MyFiles from "./pages/MyFiles";
import SharedDownloadPage from "./pages/SharedDownloadPage";
import EditFilePage from "./pages/EditFilePage";

function App() {

  return (
    <>
      <Toaster position="top-right" richColors closeButton />
      <BrowserRouter>
        <Routes>
          {/* Public Routes */}
          <Route path="/auth" element={<AuthPage />} />

          <Route path="/share/:fileId" element={<SharedDownloadPage />} />

          {/* Protected Routes */}

          <Route element={<ProtectedRoute />}>
            <Route path="/" element={<Home/>} />
          </Route>


          <Route element={<ProtectedRoute />}>
            <Route path="/my-files" element={<MyFiles/>} />
          </Route>

          <Route element={<ProtectedRoute />}>
            <Route path="/my-files/:fileId/edit" element={<EditFilePage/>} />
          </Route>
        </Routes>
      </BrowserRouter>
    </>
  )
}

export default App
