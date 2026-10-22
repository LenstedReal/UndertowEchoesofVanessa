import { createFileRoute } from "@tanstack/react-router"
import { HorrorGame } from "@/components/HorrorGame"

export const Route = createFileRoute("/")({ component: Home })

function Home() {
  return <HorrorGame />
}
