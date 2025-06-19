import { IdGenerator } from "../IdGenerator";
import { useSessionStorageState } from "./useSessionStorageState";
import { useLifeCycle } from "./useLifeCycle";

export const useUserId = (defaultUserIdGenerator) => {
    const [userId, setUserId] = useSessionStorageState('userId');

    useLifeCycle({
        onMount: () => {
            const userIdGenerator = defaultUserIdGenerator || IdGenerator;
            !userId && setUserId(userIdGenerator());
        }
    });

    return userId;
};
