import { createRoot } from "react-dom/client";
import type { AlertButton, AlertOptions } from "react-native";
import { Pressable, View } from "react-native";
import { cn } from "~/utils";
import { Heading, P } from "./text";

const AlertDialog = ({
	title,
	message,
	buttons,
	onDismiss,
}: {
	title: string;
	message?: string;
	buttons: AlertButton[];
	onDismiss: () => void;
}) => {
	return (
		<Pressable
			className="absolute inset-0 cursor-default! items-center justify-center bg-black/60"
			onPress={onDismiss}
		>
			<Pressable
				className={cn(
					"w-full max-w-md rounded-md bg-background",
					"cursor-default! overflow-hidden",
				)}
				onPress={(e) => e.preventDefault()}
			>
				<View className="items-center gap-2 p-6">
					<Heading>{title}</Heading>
					{message && <P className="text-center">{message}</P>}
				</View>
				<View className="flex-row justify-end gap-2 p-4">
					{buttons.map((button, i) => (
						<Pressable
							key={i}
							onPress={() => {
								onDismiss();
								button.onPress?.();
							}}
							className={cn(
								"rounded px-4 py-2",
								button.style === "destructive"
									? "bg-red-700 hover:bg-red-500 focus:bg-red-500"
									: button.style === "cancel"
										? "hover:bg-popover focus:bg-popover"
										: "bg-accent hover:bg-accent/50 focus:bg-accent/50",
							)}
						>
							<P
								className={cn(
									button.style === "destructive" &&
										"text-slate-800 dark:text-slate-800",
									button.style === "cancel" &&
										"text-slate-600 dark:text-slate-400",
									button.style === "default" &&
										"text-slate-200 dark:text-slate-200",
								)}
							>
								{button.text ?? "OK"}
							</P>
						</Pressable>
					))}
				</View>
			</Pressable>
		</Pressable>
	);
};

// biome-ignore lint/complexity/noStaticOnlyClass: Compatibility with rn
export class Alert {
	static alert(
		title: string,
		message?: string,
		buttons?: AlertButton[],
		options?: AlertOptions,
	): void {
		const resolvedButtons = buttons ?? [
			{ text: "OK", style: "default" as const },
		];

		const container = document.createElement("div");
		container.style.position = "fixed";
		container.style.inset = "0";
		container.style.zIndex = "9999";
		document.body.appendChild(container);

		const root = createRoot(container);

		const dismiss = () => {
			root.unmount();
			container.remove();
		};

		const cancelButton = resolvedButtons.find((b) => b.style === "cancel");

		root.render(
			<AlertDialog
				title={title}
				message={message}
				buttons={resolvedButtons}
				onDismiss={() => {
					if (options?.cancelable !== false) {
						dismiss();
						cancelButton?.onPress?.();
					}
				}}
			/>,
		);
	}
}
