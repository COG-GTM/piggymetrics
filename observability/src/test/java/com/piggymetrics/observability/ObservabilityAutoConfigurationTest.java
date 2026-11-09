package com.piggymetrics.observability;

import org.junit.Test;
import org.springframework.boot.autoconfigure.AutoConfigurations;
import org.springframework.boot.test.context.runner.ApplicationContextRunner;
import org.springframework.boot.test.context.runner.ReactiveWebApplicationContextRunner;
import org.springframework.boot.test.context.runner.WebApplicationContextRunner;
import org.springframework.boot.web.servlet.FilterRegistrationBean;
import org.springframework.core.Ordered;

import static org.assertj.core.api.Assertions.assertThat;

public class ObservabilityAutoConfigurationTest {

	private final AutoConfigurations config = AutoConfigurations.of(ObservabilityAutoConfiguration.class);

	@Test
	public void shouldRegisterServletFilterFirstInServletApplications() {
		new WebApplicationContextRunner().withConfiguration(config).run(context -> {
			FilterRegistrationBean<?> registration =
					context.getBean("correlationIdFilterRegistration", FilterRegistrationBean.class);
			assertThat(registration.getFilter()).isInstanceOf(CorrelationIdFilter.class);
			assertThat(registration.getOrder()).isEqualTo(Ordered.HIGHEST_PRECEDENCE);
			assertThat(context).doesNotHaveBean(CorrelationIdWebFilter.class);
		});
	}

	@Test
	public void shouldRegisterWebFilterInReactiveApplications() {
		new ReactiveWebApplicationContextRunner().withConfiguration(config).run(context -> {
			assertThat(context).hasSingleBean(CorrelationIdWebFilter.class);
			assertThat(context).doesNotHaveBean("correlationIdFilterRegistration");
		});
	}

	@Test
	public void shouldRegisterNothingOutsideWebApplications() {
		new ApplicationContextRunner().withConfiguration(config).run(context -> {
			assertThat(context).doesNotHaveBean(CorrelationIdWebFilter.class);
			assertThat(context).doesNotHaveBean("correlationIdFilterRegistration");
		});
	}
}
