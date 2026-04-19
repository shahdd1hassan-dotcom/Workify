/* =========================
   NAVIGATION
========================= */

function go(page){
  window.location.href = page;
}

document.addEventListener("DOMContentLoaded", function(){
  document.querySelectorAll("[data-go]").forEach((item) => {
    item.addEventListener("click", () => go(item.dataset.go));
    item.addEventListener("keydown", (e) => {
      if (e.key === "Enter" || e.key === " ") {
        e.preventDefault();
        go(item.dataset.go);
      }
    });
  });

  document.querySelectorAll("[data-alert]").forEach((btn) => {
    btn.addEventListener("click", () => alert(btn.dataset.alert));
  });

  document.querySelectorAll("[data-action]").forEach((btn) => {
    btn.addEventListener("click", () => {
      const action = btn.dataset.action;
      if (action === "shortlist") shortlist();
      if (action === "message") message();
      if (action === "decline") decline();
      if (action === "hire") hire();
    });
  });
});

/* =========================
   PROPOSALS ACTIONS
========================= */

function shortlist(){
  alert("Freelancer added to shortlist ⭐");
}

function message(){
  alert("Open chat 💬");
}

function decline(){
  alert("Declined ❌");
}

function hire(){
  alert("Hired 🎉");
}

function confirmHire(){
  alert("Hired 🎉");
}

/* =========================
   POST JOB STEPS (SAFE)
========================= */

document.addEventListener("DOMContentLoaded", function(){

  const wizard = document.querySelector(".wizard");
  if(!wizard) return;

  let currentStep = 0;

  const steps = document.querySelectorAll(".wizard .step");
  const indicators = document.querySelectorAll(".step-item");

  function showStep(){
    steps.forEach((step,index)=>{
      step.classList.remove("active");
      if(index === currentStep){
        step.classList.add("active");
      }
    });

    indicators.forEach((item,index)=>{
      item.classList.remove("active");
      if(index === currentStep){
        item.classList.add("active");
      }
    });
  }

  function nextStep(){
    if(currentStep < steps.length - 1){
      currentStep++;
      showStep();
    }else{
      alert("Job Posted Successfully 🎉");
    }
  }

  function prevStep(){
    if(currentStep > 0){
      currentStep--;
      showStep();
    }
  }

  const nextBtn = document.querySelector(".next-btn");
  const backBtn = document.querySelector(".back-btn");

  if(nextBtn){
    nextBtn.addEventListener("click", nextStep);
  }

  if(backBtn){
    backBtn.addEventListener("click", prevStep);
  }

  showStep();
});

/* =========================
   HIRE BUTTON (SAFE)
========================= */

document.addEventListener("DOMContentLoaded", function(){
  const hireBtn = document.querySelector(".hire-btn");

  if(hireBtn){
    hireBtn.addEventListener("click", () => {
      alert("🎉 Freelancer Hired Successfully!");
    });
  }
});