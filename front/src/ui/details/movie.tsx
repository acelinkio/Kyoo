import { useState } from "react";
import Animated from "react-native-reanimated";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import { useQueryState } from "~/utils";
import { HeaderBackground, useScrollNavbar } from "../navbar";
import { Header } from "./header";
import { Staff } from "./staff";

export const MovieDetails = () => {
	const [slug] = useQueryState("slug", undefined!);
	const insets = useSafeAreaInsets();
	const [imageHeight, setHeight] = useState(300);
	const { scrollHandler, headerProps } = useScrollNavbar({ imageHeight });

	return (
		<>
			<HeaderBackground {...headerProps} />
			<Animated.ScrollView
				onScroll={scrollHandler}
				scrollEventThrottle={16}
				contentContainerStyle={{ paddingBottom: insets.bottom }}
			>
				<Header
					kind="movie"
					slug={slug}
					onImageLayout={(e) => setHeight(e.nativeEvent.layout.height)}
				/>
				<Staff kind="movie" slug={slug} />
			</Animated.ScrollView>
		</>
	);
};
