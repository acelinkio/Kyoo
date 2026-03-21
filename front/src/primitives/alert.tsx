// Stolen from https://github.com/necolas/react-native-web/issues/1026#issuecomment-1458279681

import {
	type AlertButton,
	type AlertOptions,
	Alert as RNAlert,
} from "react-native";

export interface ExtendedAlertStatic {
	alert: (
		title: string,
		message?: string,
		buttons?: AlertButton[],
		options?: AlertOptions,
	) => void;
}

export const Alert: ExtendedAlertStatic = RNAlert as ExtendedAlertStatic;
