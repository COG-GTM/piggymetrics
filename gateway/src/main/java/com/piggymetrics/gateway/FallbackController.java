package com.piggymetrics.gateway;

import org.springframework.http.HttpStatus;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RestController;
import org.springframework.web.server.ServerWebExchange;
import reactor.core.publisher.Mono;

@RestController
public class FallbackController {

	@GetMapping("/fallback")
	public Mono<String> fallback(ServerWebExchange exchange) {
		exchange.getResponse().setStatusCode(HttpStatus.SERVICE_UNAVAILABLE);
		return Mono.just("Service temporarily unavailable. Please try again later.");
	}
}
