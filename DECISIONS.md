# Decisions

## 1. Key design decisions

- **Three projects.** Application holds the models, interfaces and `VehicleService`, and has no
  references. Infrastructure implements the data provider by calling NHTSA. Api wires everything
  together in `Program.cs`. NHTSA details stay in one place.
- **One models endpoint.** NHTSA has two model endpoints, with and without a vehicle type. The API
  exposes one, `GetModels`, where `vehicleType` is optional.
- **Clean response shapes.** NHTSA's field names (`Make_ID`, `Model_Name`) are read by `internal`
  classes in Infrastructure and mapped to small records with `id` and `name`.
- **Caching.** Successful NHTSA responses are cached in memory for 24 hours (configurable).
  Failures are never cached.
- **Resilience.** The NHTSA client uses the standard resilience handler with longer timeouts
  (20 s per attempt, 60 s in total), because the makes list has over 12,000 rows and vPIC can be slow.
- **Central error handling.** One exception handler returns ProblemDetails: 400 for bad input,
  502 when NHTSA fails, 500 for anything else. Controllers have no try/catch.
- **Rate limiting.** 100 requests per minute per client IP, so the API can't be used to hammer NHTSA.
- **Frontend.** One page. The page component owns the state as signals, and `switchMap` cancels
  stale requests. Makes are loaded once and filtered in the browser, with at most 50 suggestions.
- **One container.** The API serves the Angular build, so the app runs on one origin with no CORS.

## 2. Assumptions

- Model years go from 1995 to next year.
- NHTSA matches the vehicle type by partial name ("car" returns "Passenger Car"), so the UI sends
  the full name.
- An empty result is not an error: the API returns 200 with `[]`.
- vPIC data changes rarely, so a 24-hour cache is acceptable.
- There is no proxy or load balancer in front of the API, so the connection IP is the client IP.

## 3. Trade-offs

- **Three layers for a small app.** One project would also have worked. More files, in exchange
  for keeping NHTSA out of the business rules.
- **Action-name routes.** `GetMakes`, `GetVehicleTypes` and `GetModels` mirror the NHTSA
  operations. Simple for three read-only queries, but RPC-style; a larger API would use REST routes.
- **No automated tests.** Time went to the required features. The service stays testable because
  the data provider and the clock are injected.
- **In-memory cache.** Lost on restart and not shared between instances.
- **All makes sent to the browser.** Typing is instant, but a server-side search would be lighter.
- **PrimeNG.** Saves UI work, but the initial bundle is about 1 MB.
- **Plain HTTP only.**

## 4. Next steps

- Automated tests: `VehicleService`, the NHTSA provider, and one end-to-end test of the search flow.
- HTTPS with a domain.
- CI that builds and publishes the Docker image, so the server pulls it from a registry.
- Server-side make search, and a distributed cache if the app is scaled out.

## 5. Deployment

One Docker container on a single AWS EC2 instance (t3.micro, Amazon Linux 2023) under the free plan.

- I built the image locally, exported it with `docker save`, copied it to the instance over SSH
  and loaded it with `docker load`. No registry and no AWS access keys, so there are no
  credentials to store or leak.
- The container runs with `--restart unless-stopped` and maps port 80 to 8080.
- The security group allows HTTP from anywhere and SSH only from my own IP. An Elastic IP keeps
  the public address fixed.

## 6. How I used AI

I work with .NET and Angular every day, so the backend and frontend are things I could have
written by hand. I used Claude Code to write some code faster, one step at a
time. After each step I read the changes and ran the app before moving on.

I did not follow every suggestion. Claude proposed a single project organized in folders, because
the app is small. I chose three separate projects, following Clean Architecture, so the
dependency direction is enforced by project references and not just by convention. The routes
and the scope (no tests, no authentication) were also my decisions. I also checked the NHTSA
responses myself in the browser before any code was written, which is how I confirmed that the
two model endpoints return the same shape and can be served by one endpoint with an optional
vehicle type.

AI helped me most with the UI design, the exception handling and caching, and with Docker and
AWS, which were both new to me: all my deployment experience is IIS on Windows Server. I also
used it to draft this document and the README, and then edited them.

**One thing it got wrong that I fixed.** PrimeNG's theme follows the operating system's dark mode
by default, but the page was written for a light theme only. The AI tested in light mode, so its
checks passed. On my machine, which is in dark mode, the cards were dark inside a light page. I
set `darkModeSelector: 'none'` so the app always uses the light theme.