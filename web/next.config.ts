import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Next.js writes an AGENTS.md into the project on dev and build. Nothing here
  // reads it, so it is turned off rather than committed or ignored.
  agentRules: false,
};

export default nextConfig;
