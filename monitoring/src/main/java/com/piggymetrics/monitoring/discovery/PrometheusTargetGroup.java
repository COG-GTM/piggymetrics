package com.piggymetrics.monitoring.discovery;

import java.util.List;
import java.util.Map;

/**
 * A single entry of the Prometheus {@code http_sd_config} response document.
 */
public record PrometheusTargetGroup(List<String> targets, Map<String, String> labels) {
}
