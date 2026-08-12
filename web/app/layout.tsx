import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import Link from "next/link";
import "./globals.css";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "Fundo — Apply for a loan",
  description: "Apply for working capital in a few minutes.",
};

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html
      lang="en"
      className={`${geistSans.variable} ${geistMono.variable} h-full antialiased`}
    >
      <body className="font-sans min-h-full flex flex-col">
        <header className="border-b border-border bg-surface">
          <div className="mx-auto flex max-w-3xl items-center gap-3 px-6 py-4">
            <Link href="/" className="text-lg font-semibold tracking-tight">
              Fundo
            </Link>
            <span className="text-sm text-muted">Business lending</span>
          </div>
        </header>

        <main className="mx-auto w-full max-w-3xl flex-1 px-6 py-10">{children}</main>

        <footer className="border-t border-border">
          <div className="mx-auto max-w-3xl px-6 py-6 text-xs text-muted">
            Take-home demo. No real credit decision is made and no data leaves your machine.
          </div>
        </footer>
      </body>
    </html>
  );
}
