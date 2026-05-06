package com.piggymetrics.turbine;

import io.micrometer.core.instrument.MeterRegistry;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

import java.util.function.Consumer;

/**
 * Replaces Netflix Turbine stream aggregation with Micrometer-based metrics consumption.
 * Listens on RabbitMQ for metrics messages from downstream services via Spring Cloud Stream.
 */
@Configuration
public class MetricsAggregationConfig {

    private static final Logger log = LoggerFactory.getLogger(MetricsAggregationConfig.class);

    @Bean
    public Consumer<String> metricsConsumer(MeterRegistry meterRegistry) {
        return message -> {
            log.debug("Received metrics message: {}", message);
            meterRegistry.counter("turbine.metrics.received").increment();
        };
    }
}
