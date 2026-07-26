import { useAuthStore } from "../stores/useAuthStore";
import Logout from "@/components/auth/Logout";

const Home = () => {
    const email = useAuthStore(state => state.email);
    return (
        <div>
            <Logout/>
            <h1>Welcome, {email}!</h1>
        </div>
    );
};

export default Home;