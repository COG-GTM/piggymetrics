package com.piggymetrics.observability;

import org.springframework.boot.autoconfigure.condition.ConditionalOnClass;
import org.springframework.boot.autoconfigure.condition.ConditionalOnMissingBean;
import org.springframework.boot.autoconfigure.condition.ConditionalOnWebApplication;
import org.springframework.boot.autoconfigure.condition.ConditionalOnWebApplication.Type;
import org.springframework.boot.web.servlet.FilterRegistrationBean;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.core.Ordered;
import org.springframework.core.annotation.Order;

/**
 * Registers the correlation ID filter for servlet and reactive web applications.
 */
@Configuration
public class ObservabilityAutoConfiguration {

	@Configuration
	@ConditionalOnWebApplication(type = Type.SERVLET)
	@ConditionalOnClass(name = "javax.servlet.Filter")
	static class ServletCorrelationIdConfiguration {

		@Bean
		@ConditionalOnMissingBean(name = "correlationIdFilterRegistration")
		public FilterRegistrationBean<CorrelationIdFilter> correlationIdFilterRegistration() {
			FilterRegistrationBean<CorrelationIdFilter> registration =
					new FilterRegistrationBean<>(new CorrelationIdFilter());
			registration.setName("correlationIdFilter");
			registration.setOrder(Ordered.HIGHEST_PRECEDENCE);
			return registration;
		}
	}

	@Configuration
	@ConditionalOnWebApplication(type = Type.REACTIVE)
	@ConditionalOnClass(name = "org.springframework.web.server.WebFilter")
	static class ReactiveCorrelationIdConfiguration {

		@Bean
		@Order(Ordered.HIGHEST_PRECEDENCE)
		@ConditionalOnMissingBean
		public CorrelationIdWebFilter correlationIdWebFilter() {
			return new CorrelationIdWebFilter();
		}
	}
}
