import { HR as EHR } from "@expo/html-elements";
import { View } from "react-native";
import { cn } from "~/utils";
import { P } from "./text";

export const HR = ({
	orientation = "horizontal",
	className,
	...props
}: {
	orientation?: "vertical" | "horizontal";
	className?: string;
}) => {
	return (
		<EHR
			className={cn(
				"shrink-0 border-0 bg-gray-400 opacity-70",
				orientation === "vertical" && "mx-4 my-2 h-auto w-px",
				orientation === "horizontal" && "mx-2 my-4 h-px w-auto",
				className,
			)}
			{...props}
		/>
	);
};

export const HRP = ({ text }: { text: string }) => {
	return (
		<View className="my-2 w-full flex-row items-center">
			<HR className="grow" />
			<P className="uppercase">{text}</P>
			<HR className="grow" />
		</View>
	);
};
