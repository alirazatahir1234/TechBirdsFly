import Image from "next/image";
import { cn } from "@/lib/utils";

type LogoVariant = "icon" | "horizontal" | "text";
type LogoSize = "sm" | "md" | "lg";

interface AppLogoProps {
  variant?: LogoVariant;
  size?: LogoSize;
  className?: string;
  showText?: boolean;
}

/**
 * Reusable TechBirdsFly Logo Component
 *
 * Variants:
 * - icon: Just the bird icon (square)
 * - horizontal: Bird + Text side by side
 * - text: Just the text
 *
 * Sizes:
 * - sm: 32x32px (icon), 120px (horizontal)
 * - md: 48x48px (icon), 150px (horizontal)
 * - lg: 64x64px (icon), 200px (horizontal)
 */

export function AppLogo({
  variant = "icon",
  size = "md",
  className,
  showText = false,
}: AppLogoProps) {
  const iconSizes: Record<LogoSize, { width: number; height: number }> = {
    sm: { width: 32, height: 32 },
    md: { width: 48, height: 48 },
    lg: { width: 64, height: 64 },
  };

  const horizontalSizes: Record<LogoSize, { width: number; height: number }> = {
    sm: { width: 120, height: 40 },
    md: { width: 150, height: 50 },
    lg: { width: 200, height: 67 },
  };

  const fontSizeMap: Record<LogoSize, string> = {
    sm: "16px",
    md: "20px",
    lg: "24px",
  };

  if (variant === "icon") {
    const dimensions = iconSizes[size];
    return (
      <div
        className={cn(
          "flex items-center justify-center rounded-md",
          className
        )}
        style={{
          width: dimensions.width,
          height: dimensions.height,
        }}
      >
        <Image
          src="/images/techbirdsfly/techbirdsfly.svg"
          alt="TechBirdsFly"
          width={dimensions.width}
          height={dimensions.height}
          className="object-contain"
          priority
        />
      </div>
    );
  }

  if (variant === "horizontal") {
    const dimensions = horizontalSizes[size];
    return (
      <div className={cn("flex items-center gap-2", className)}>
        <div
          className="shrink-0 rounded-md p-1"
          style={{
            width: dimensions.height,
            height: dimensions.height,
          }}
        >
          <Image
            src="/images/techbirdsfly/techbirdsfly.svg"
            alt="TechBirdsFly"
            width={dimensions.height - 8}
            height={dimensions.height - 8}
            className="object-contain"
            priority
          />
        </div>
        <span
          className="font-bold text-purple-700 dark:text-purple-400"
          style={{
            fontSize: fontSizeMap[size],
          }}
        >
          TechBirdsFly
        </span>
      </div>
    );
  }

  if (variant === "text") {
    return (
      <span
        className={cn("font-bold text-purple-700 dark:text-purple-400", className)}
        style={{
          fontSize: fontSizeMap[size],
        }}
      >
        TechBirdsFly
      </span>
    );
  }

  return null;
}

/**
 * Logo Icon Only - Quick use
 */
export function AppLogoIcon({
  size = "md",
  className,
}: Omit<AppLogoProps, "variant" | "showText">) {
  return <AppLogo variant="icon" size={size} className={className} />;
}

/**
 * Logo with Text - For headers/navbars
 */
export function AppLogoHorizontal({
  size = "md",
  className,
}: Omit<AppLogoProps, "variant" | "showText">) {
  return <AppLogo variant="horizontal" size={size} className={className} />;
}

/**
 * Text Only - For subtle branding
 */
export function AppLogoText({
  size = "md",
  className,
}: Omit<AppLogoProps, "variant" | "showText">) {
  return <AppLogo variant="text" size={size} className={className} />;
}
