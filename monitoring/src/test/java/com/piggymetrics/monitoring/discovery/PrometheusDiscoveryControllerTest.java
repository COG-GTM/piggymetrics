package com.piggymetrics.monitoring.discovery;

import java.util.List;

import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.springframework.cloud.client.DefaultServiceInstance;
import org.springframework.cloud.client.discovery.DiscoveryClient;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.ArgumentMatchers.anyString;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class PrometheusDiscoveryControllerTest {

	private DiscoveryClient discoveryClient;
	private PrometheusDiscoveryController controller;

	@BeforeEach
	public void setup() {
		discoveryClient = mock(DiscoveryClient.class);
		when(discoveryClient.getInstances(anyString())).thenReturn(List.of());
		controller = new PrometheusDiscoveryController(discoveryClient, "/actuator/prometheus");
	}

	@Test
	public void shouldExposeEveryRegisteredInstanceAsScrapeTarget() {
		when(discoveryClient.getServices()).thenReturn(List.of("ACCOUNT-SERVICE"));
		when(discoveryClient.getInstances("ACCOUNT-SERVICE")).thenReturn(List.of(
				new DefaultServiceInstance("account-service:6000", "ACCOUNT-SERVICE", "account-service", 6000, false),
				new DefaultServiceInstance("account-service:6001", "ACCOUNT-SERVICE", "account-service", 6001, false)));

		List<PrometheusTargetGroup> groups = controller.targets();

		assertThat(groups).hasSize(2);
		assertThat(groups.get(0).targets()).containsExactly("account-service:6000");
		assertThat(groups.get(0).labels())
				.containsEntry("job", "account-service")
				.containsEntry("application", "account-service")
				.containsEntry("instance_id", "account-service:6000")
				.containsEntry("__scheme__", "http")
				.containsEntry("__metrics_path__", "/actuator/prometheus");
		assertThat(groups.get(1).targets()).containsExactly("account-service:6001");
	}

	@Test
	public void shouldUseHttpsSchemeForSecureInstances() {
		when(discoveryClient.getServices()).thenReturn(List.of("STATISTICS-SERVICE"));
		when(discoveryClient.getInstances("STATISTICS-SERVICE")).thenReturn(List.of(
				new DefaultServiceInstance("statistics-service:7000", "STATISTICS-SERVICE", "statistics-service", 7000,
						true)));

		List<PrometheusTargetGroup> groups = controller.targets();

		assertThat(groups).hasSize(1);
		assertThat(groups.get(0).labels()).containsEntry("__scheme__", "https");
	}

	@Test
	public void shouldReturnEmptyDocumentWhenNothingIsRegistered() {
		when(discoveryClient.getServices()).thenReturn(List.of());

		assertThat(controller.targets()).isEmpty();
	}
}
