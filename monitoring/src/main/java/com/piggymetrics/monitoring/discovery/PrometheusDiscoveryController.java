package com.piggymetrics.monitoring.discovery;

import java.util.LinkedHashMap;
import java.util.List;
import java.util.Locale;
import java.util.Map;

import org.springframework.beans.factory.annotation.Value;
import org.springframework.cloud.client.ServiceInstance;
import org.springframework.cloud.client.discovery.DiscoveryClient;
import org.springframework.http.MediaType;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

/**
 * Publishes every instance registered in Eureka as a Prometheus
 * {@code http_sd_config} target, so that Prometheus scrapes the actuator
 * Prometheus endpoint of each service instance as it comes and goes.
 */
@RestController
@RequestMapping("/discovery")
public class PrometheusDiscoveryController {

	private final DiscoveryClient discoveryClient;
	private final String metricsPath;

	public PrometheusDiscoveryController(DiscoveryClient discoveryClient,
			@Value("${monitoring.prometheus.metrics-path:/actuator/prometheus}") String metricsPath) {
		this.discoveryClient = discoveryClient;
		this.metricsPath = metricsPath;
	}

	@GetMapping(value = "/targets", produces = MediaType.APPLICATION_JSON_VALUE)
	public List<PrometheusTargetGroup> targets() {
		return discoveryClient.getServices().stream()
				.flatMap(serviceId -> discoveryClient.getInstances(serviceId).stream())
				.map(this::toTargetGroup)
				.toList();
	}

	private PrometheusTargetGroup toTargetGroup(ServiceInstance instance) {
		String serviceId = instance.getServiceId().toLowerCase(Locale.ROOT);
		String target = instance.getHost() + ":" + instance.getPort();

		Map<String, String> labels = new LinkedHashMap<>();
		labels.put("job", serviceId);
		labels.put("application", serviceId);
		labels.put("instance_id", instance.getInstanceId() != null ? instance.getInstanceId() : target);
		labels.put("__scheme__", instance.isSecure() ? "https" : "http");
		labels.put("__metrics_path__", metricsPath);

		return new PrometheusTargetGroup(List.of(target), labels);
	}
}
