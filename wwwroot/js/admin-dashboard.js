document.addEventListener("DOMContentLoaded", function () {
	if (!window.adminAuth || !window.adminAuth.isAuthed()) {
		return;
	}

	var state = {
		activeTab: "overview",
		search: "",
		users: [
			{ name: "John Doe", email: "john@workify.com", role: "Client", status: "verified", lastSeen: "2m ago", risk: "low" },
			{ name: "Alex Karim", email: "alex@workify.com", role: "Freelancer", status: "review", lastSeen: "14m ago", risk: "medium" },
			{ name: "Sarah Lee", email: "sarah@workify.com", role: "Client", status: "banned", lastSeen: "1h ago", risk: "high" },
			{ name: "Mina Hart", email: "mina@workify.com", role: "Freelancer", status: "verified", lastSeen: "3h ago", risk: "low" },
		],
		jobs: [
			{ title: "Senior React Developer for Fintech App", reporter: "John Doe", reason: "Off-platform payment request", severity: "high" },
			{ title: "UX Designer for Dashboard Redesign", reporter: "Mina Hart", reason: "Duplicate posting", severity: "medium" },
			{ title: "Technical Writer for API Docs", reporter: "Ops Flag", reason: "Suspicious keyword stuffing", severity: "low" },
		],
		finance: [
			{ label: "Total Escrow Held", value: "$84,200", state: "healthy" },
			{ label: "Fees Collected (MTD)", value: "$12,480", state: "growing" },
			{ label: "Pending Withdrawals", value: "$6,900", state: "watch" },
		],
		watchlist: [
			{ title: "User: Sarah Lee", meta: "2 policy hits in 24h" },
			{ title: "Job: Off-platform request", meta: "Needs manual review" },
			{ title: "Withdrawal batch #204", meta: "Awaiting approval" },
		],
		activity: [
			{ title: "Escrow released for Contract #CX-2041", meta: "5 minutes ago" },
			{ title: "Verified user Mina Hart", meta: "18 minutes ago" },
			{ title: "Removed spam job posting", meta: "1 hour ago" },
			{ title: "Pending withdrawal flagged", meta: "2 hours ago" },
		],
	};

	var tabButtons = document.querySelectorAll("[data-admin-tab]");
	var panels = document.querySelectorAll("[data-admin-panel]");
	var logoutButton = document.getElementById("admin-logout");
	var refreshButton = document.getElementById("admin-refresh");
	var searchInput = document.getElementById("admin-search");
	var jumpButtons = document.querySelectorAll("[data-jump-tab]");

	function formatStatus(status) {
		return status.charAt(0).toUpperCase() + status.slice(1);
	}

	function getStatusClass(status) {
		if (status === "verified" || status === "healthy" || status === "growing") return "success";
		if (status === "review" || status === "watch" || status === "medium") return "warning";
		if (status === "banned" || status === "high") return "danger";
		return "neutral";
	}

	function setTab(tabName) {
		state.activeTab = tabName;

		tabButtons.forEach(function (button) {
			button.classList.toggle("active", button.getAttribute("data-admin-tab") === tabName);
		});

		panels.forEach(function (panel) {
			panel.style.display = panel.getAttribute("data-admin-panel") === tabName ? "block" : "none";
		});
	}

	function renderUsers() {
		var tbody = document.querySelector("#usersTable tbody");
		if (!tbody) return;

		var rows = state.users.filter(function (user) {
			var haystack = [user.name, user.email, user.role, user.status, user.lastSeen].join(" ").toLowerCase();
			return haystack.indexOf(state.search.toLowerCase()) !== -1;
		});

		tbody.innerHTML = rows
			.map(function (user, index) {
				return [
					"<tr class='admin-table-row'>",
					"<td><strong>" + user.name + "</strong><div class='admin-note'>" + user.email + "</div></td>",
					"<td>" + user.role + "</td>",
					"<td><span class='admin-status " + getStatusClass(user.status) + "'>" + formatStatus(user.status) + "</span></td>",
					"<td>" + user.lastSeen + "</td>",
					"<td><div class='admin-row-actions'>",
					"<button class='admin-button secondary' type='button' data-user-action='verify' data-user-index='" + index + "'>Verify</button>",
					"<button class='admin-button secondary' type='button' data-user-action='ban' data-user-index='" + index + "'>Ban</button>",
					"<button class='admin-button secondary' type='button' data-user-action='impersonate' data-user-index='" + index + "'>Impersonate</button>",
					"</div></td>",
					"</tr>",
				].join("");
			})
			.join("");
	}

	function renderJobs() {
		var queue = document.getElementById("jobsQueue");
		if (!queue) return;

		queue.innerHTML = state.jobs
			.map(function (job, index) {
				return [
					"<div class='admin-list-item'>",
					"<div class='admin-list-copy'>",
					"<h3 class='admin-list-title'>" + job.title + "</h3>",
					"<div class='admin-list-meta'>Reporter: " + job.reporter + " • Reason: " + job.reason + "</div>",
					"</div>",
					"<div class='admin-row-actions'>",
					"<span class='admin-status " + getStatusClass(job.severity) + "'>" + formatStatus(job.severity) + "</span>",
					"<button class='admin-button secondary' type='button' data-job-action='approve' data-job-index='" + index + "'>Approve</button>",
					"<button class='admin-button secondary' type='button' data-job-action='remove' data-job-index='" + index + "'>Remove</button>",
					"<button class='admin-button secondary' type='button' data-job-action='escalate' data-job-index='" + index + "'>Escalate</button>",
					"</div>",
					"</div>",
				].join("");
			})
			.join("");
	}

	function renderFinance() {
		var tbody = document.querySelector("#financeTable tbody");
		if (!tbody) return;

		tbody.innerHTML = state.finance
			.map(function (item) {
				return [
					"<tr class='admin-table-row'>",
					"<td>" + item.label + "</td>",
					"<td><strong>" + item.value + "</strong></td>",
					"<td><span class='admin-status " + getStatusClass(item.state) + "'>" + formatStatus(item.state) + "</span></td>",
					"</tr>",
				].join("");
			})
			.join("");
	}

	function renderWatchlist() {
		var container = document.getElementById("watchlist");
		if (!container) return;

		container.innerHTML = state.watchlist
			.map(function (item) {
				return [
					"<div class='admin-list-item'>",
					"<div class='admin-list-copy'>",
					"<h3 class='admin-list-title'>" + item.title + "</h3>",
					"<div class='admin-list-meta'>" + item.meta + "</div>",
					"</div>",
					"<span class='admin-status warning'>Watch</span>",
					"</div>",
				].join("");
			})
			.join("");
	}

	function renderActivity() {
		var container = document.getElementById("activityLog");
		if (!container) return;

		container.innerHTML = state.activity
			.map(function (item) {
				return [
					"<div class='admin-log-item'>",
					"<div class='admin-log-bullet'></div>",
					"<div>",
					"<h3 class='admin-log-title'>" + item.title + "</h3>",
					"<div class='admin-log-meta'>" + item.meta + "</div>",
					"</div>",
					"</div>",
				].join("");
			})
			.join("");
	}

	function updateSummary() {
		var escrowTotal = document.getElementById("escrowTotal");
		var feesTotal = document.getElementById("feesTotal");
		var pendingTotal = document.getElementById("pendingTotal");
		var riskFlags = document.getElementById("riskFlags");

		if (escrowTotal) escrowTotal.textContent = "$84,200";
		if (feesTotal) feesTotal.textContent = "$12,480";
		if (pendingTotal) pendingTotal.textContent = "$6,900";
		if (riskFlags) riskFlags.textContent = String(state.jobs.length + 2);
	}

	function drawChart() {
		var svg = document.getElementById("financeChart");
		if (!svg) return;

		var width = 360;
		var height = 180;
		var points = [22, 26, 28, 24, 34, 32, 41, 39, 47, 52, 58, 56];
		var maxValue = Math.max.apply(Math, points);
		var minValue = Math.min.apply(Math, points);
		var plotWidth = width - 32;
		var plotHeight = height - 32;
		var step = plotWidth / (points.length - 1);

		var path = points
			.map(function (value, index) {
				var x = 16 + index * step;
				var y = 16 + plotHeight - ((value - minValue) / (maxValue - minValue || 1)) * plotHeight;
				return (index === 0 ? "M" : "L") + x + " " + y;
			})
			.join(" ");

		svg.innerHTML = [
			"<defs>",
			"<linearGradient id='adminLine' x1='0' y1='0' x2='1' y2='1'>",
			"<stop offset='0%' stop-color='#e11d48' />",
			"<stop offset='100%' stop-color='#fb7185' />",
			"</linearGradient>",
			"<linearGradient id='adminFill' x1='0' y1='0' x2='0' y2='1'>",
			"<stop offset='0%' stop-color='rgba(225,29,72,0.28)' />",
			"<stop offset='100%' stop-color='rgba(225,29,72,0.02)' />",
			"</linearGradient>",
			"</defs>",
			"<rect x='0' y='0' width='360' height='180' rx='18' fill='rgba(255,255,255,0.02)' />",
			"<path d='" + path + "' fill='none' stroke='url(#adminLine)' stroke-width='4' stroke-linecap='round' stroke-linejoin='round' />",
			"<path d='" + path + " L " + (16 + (points.length - 1) * step) + " 164 L 16 164 Z' fill='url(#adminFill)' opacity='0.45' />",
		].join("");
	}

	function updateCounts() {
		updateSummary();
	}

	function handleUserAction(button) {
		var index = Number(button.getAttribute("data-user-index"));
		var action = button.getAttribute("data-user-action");
		var user = state.users[index];
		if (!user) return;

		if (action === "verify") {
			user.status = "verified";
			state.activity.unshift({ title: "Verified user " + user.name, meta: "Just now" });
		}

		if (action === "ban") {
			user.status = "banned";
			state.activity.unshift({ title: "Banned user " + user.name, meta: "Just now" });
		}

		if (action === "impersonate") {
			state.activity.unshift({ title: "Impersonation preview for " + user.name, meta: "Just now" });
			window.alert("Impersonation is a UI action only in this demo.");
		}

		renderUsers();
		renderActivity();
	}

	function handleJobAction(button) {
		var index = Number(button.getAttribute("data-job-index"));
		var action = button.getAttribute("data-job-action");
		var job = state.jobs[index];
		if (!job) return;

		if (action === "approve") {
			state.finance[0].value = "$84,200";
			state.finance[1].value = "$12,480";
			state.activity.unshift({ title: "Approved job " + job.title, meta: "Just now" });
		}

		if (action === "remove") {
			state.jobs.splice(index, 1);
			state.activity.unshift({ title: "Removed job " + job.title, meta: "Just now" });
		}

		if (action === "escalate") {
			state.activity.unshift({ title: "Escalated job " + job.title, meta: "Just now" });
		}

		renderJobs();
		renderActivity();
		updateCounts();
	}

	tabButtons.forEach(function (button) {
		button.addEventListener("click", function (event) {
			event.preventDefault();
			setTab(button.getAttribute("data-admin-tab"));
		});
	});

	jumpButtons.forEach(function (button) {
		button.addEventListener("click", function () {
			setTab(button.getAttribute("data-jump-tab"));
		});
	});

	document.addEventListener("click", function (event) {
		var userButton = event.target.closest("[data-user-action]");
		if (userButton) {
			handleUserAction(userButton);
			return;
		}

		var jobButton = event.target.closest("[data-job-action]");
		if (jobButton) {
			handleJobAction(jobButton);
		}
	});

	if (searchInput) {
		searchInput.addEventListener("input", function () {
			state.search = searchInput.value;
			renderUsers();
		});
	}

	if (logoutButton) {
		logoutButton.addEventListener("click", function () {
			window.adminAuth.handleLogout();
		});
	}

	if (refreshButton) {
		refreshButton.addEventListener("click", function () {
			renderUsers();
			renderJobs();
			renderFinance();
			renderWatchlist();
			renderActivity();
			updateCounts();
			drawChart();
		});
	}

	renderUsers();
	renderJobs();
	renderFinance();
	renderWatchlist();
	renderActivity();
	updateCounts();
	drawChart();
	setTab("overview");

	if (typeof lucide !== "undefined") {
		lucide.createIcons();
	}
});