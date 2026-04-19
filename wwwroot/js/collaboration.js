/* =============================================================
   COLLABORATION.JS — Shared-pages interactivity
   Handles: tabs, modals, chat send, and chat thread switching.
   Runs AFTER shared.js (which loads the sidebar & header).
   ============================================================= */

document.addEventListener('DOMContentLoaded', function () {

  /* ─────────────────────────────────────────────────────────────
     HELPERS
     ───────────────────────────────────────────────────────────── */
  function $(sel, root) { return (root || document).querySelector(sel); }
  function $all(sel, root) { return Array.from((root || document).querySelectorAll(sel)); }

  function formatTime(d) {
    return String(d.getHours()).padStart(2, '0') + ':' + String(d.getMinutes()).padStart(2, '0');
  }


  /* ─────────────────────────────────────────────────────────────
     PILL / TAB FILTERING
     Usage:  add data-tabs to the wrapper, data-tab="true" to each
     button, data-filter="VALUE" to the button, and
     data-filter-item="VALUE" to each filterable row.
     ───────────────────────────────────────────────────────────── */
  $all('[data-tabs]').forEach(function (tabsWrap) {
    var buttons = $all('[data-tab="true"]', tabsWrap);

    // The filterable items live in the next sibling OR anywhere in
    // the nearest ancestor that contains items.
    var container = tabsWrap.closest('.collab-card-body') ||
                    tabsWrap.closest('.collab-card') ||
                    tabsWrap.parentElement;

    buttons.forEach(function (btn) {
      btn.addEventListener('click', function () {
        // Update selection state
        buttons.forEach(function (b) {
          b.setAttribute('aria-selected', 'false');
          b.classList.remove('active');
        });
        btn.setAttribute('aria-selected', 'true');
        btn.classList.add('active');

        var filter = btn.getAttribute('data-filter');

        $all('[data-filter-item]', container).forEach(function (item) {
          var val = item.getAttribute('data-filter-item');
          item.style.display = (filter === 'all' || val === filter) ? '' : 'none';
        });
      });
    });
  });


  /* ─────────────────────────────────────────────────────────────
     MODALS
     Open:   any element with  data-modal-open="#id"
     Close:  any element with  data-modal-close="#id"
             or clicking the backdrop itself.

     The modal backdrop element must have id="<id>-backdrop"
     (optional) OR we look for aria-hidden on the .modal-backdrop
     wrapping the requested id.
     ───────────────────────────────────────────────────────────── */
  function findBackdrop(modalId) {
    // Look for a backdrop that wraps the modal
    var modal = $(modalId);
    if (!modal) return null;
    var parent = modal.closest('.modal-backdrop');
    if (parent) return parent;
    // Fallback: id convention  e.g. #flag-modal → #flag-modal-backdrop
    return $(modalId + '-backdrop');
  }

  function openModal(modalId) {
    var backdrop = findBackdrop(modalId);
    if (!backdrop) return;
    backdrop.setAttribute('aria-hidden', 'false');
    backdrop.classList.add('active');
    document.body.style.overflow = 'hidden';
  }

  function closeModal(modalId) {
    var backdrop = findBackdrop(modalId);
    if (!backdrop) return;
    backdrop.setAttribute('aria-hidden', 'true');
    backdrop.classList.remove('active');
    document.body.style.overflow = '';
  }

  // Wire open buttons
  $all('[data-modal-open]').forEach(function (btn) {
    btn.addEventListener('click', function () {
      openModal(btn.getAttribute('data-modal-open'));
    });
  });

  // Wire close buttons
  $all('[data-modal-close]').forEach(function (btn) {
    btn.addEventListener('click', function () {
      closeModal(btn.getAttribute('data-modal-close'));
    });
  });

  // Click outside the modal box to close
  $all('.modal-backdrop').forEach(function (backdrop) {
    backdrop.addEventListener('click', function (e) {
      if (e.target !== backdrop) return; // only fire on backdrop itself
      // Find which modal is inside and close it
      var modal = $('[role="dialog"]', backdrop);
      if (modal) closeModal('#' + modal.id);
    });
  });


  /* ─────────────────────────────────────────────────────────────
     CHAT — MESSAGE RENDERING
     ───────────────────────────────────────────────────────────── */
  function renderMessage(messagesWrap, msg) {
    var isMe = msg.from === 'me';
    var row  = document.createElement('div');
    row.className = isMe ? 'msg-row msg-row--me' : 'msg-row';

    var avatarHtml = '<div class="collab-avatar" aria-hidden="true">' + (isMe ? 'Me' : 'U') + '</div>';
    row.innerHTML =
      (!isMe ? avatarHtml : '') +
      '<div class="msg-bubble">' +
        '<p class="msg-text"></p>' +
        '<div class="msg-time"></div>' +
      '</div>' +
      (isMe  ? avatarHtml : '');

    row.querySelector('.msg-text').textContent = msg.text;
    row.querySelector('.msg-time').textContent  = msg.time;
    messagesWrap.appendChild(row);
    messagesWrap.scrollTop = messagesWrap.scrollHeight;
  }


  /* ─────────────────────────────────────────────────────────────
     CHAT — SEND
     ───────────────────────────────────────────────────────────── */
  var messagesWrap = $('[data-chat-messages]');
  var composeInput = $('[data-chat-input]');
  var sendBtn      = $('[data-chat-send]');

  if (messagesWrap && composeInput && sendBtn && window.PAGE && window.PAGE.chat) {
    var chat = window.PAGE.chat;

    // Render initial messages for the active thread
    var initThread = chat.threads[chat.activeThreadId];
    if (initThread) {
      initThread.messages.forEach(function (m) { renderMessage(messagesWrap, m); });
    }

    function sendMessage() {
      var text = composeInput.value.trim();
      if (!text) return;
      var time     = formatTime(new Date());
      var threadId = chat.activeThreadId;
      var msg      = { from: 'me', text: text, time: time };
      chat.threads[threadId].messages.push(msg);
      renderMessage(messagesWrap, msg);
      composeInput.value = '';
      composeInput.focus();
    }

    sendBtn.addEventListener('click', sendMessage);
    composeInput.addEventListener('keydown', function (e) {
      if (e.key === 'Enter') sendMessage();
    });


    /* ───────────────────────────────────────────────────────────
       CHAT — THREAD SWITCHING
       ─────────────────────────────────────────────────────────── */
    $all('[data-thread-id]').forEach(function (btn) {
      btn.addEventListener('click', function () {
        var threadId = btn.getAttribute('data-thread-id');

        // Update active state
        $all('[data-thread-id]').forEach(function (b) {
          b.setAttribute('data-active', String(b.getAttribute('data-thread-id') === threadId));
        });

        // Switch messages
        chat.activeThreadId = threadId;
        messagesWrap.innerHTML = '';
        var thread = chat.threads[threadId];
        if (thread) {
          thread.messages.forEach(function (m) { renderMessage(messagesWrap, m); });
        }
      });
    });
  }

});
