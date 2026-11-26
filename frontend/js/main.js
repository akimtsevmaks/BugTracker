const popupStack = document.querySelector('.popup-stack');

function showPopup(message) {
  if (!popupStack) return;

  const popup = document.createElement('div');
  popup.className = 'popup-message';
  popup.textContent = message;
  popupStack.append(popup);
  popupStack.scrollTop = popupStack.scrollHeight;

  setTimeout(() => popup.remove(), 3500);
}

document.querySelectorAll('[data-open-dialog]').forEach(button => {
  button.addEventListener('click', () => {
    document.getElementById(button.dataset.openDialog).showModal();
  });
});

document.querySelectorAll('[data-close-dialog]').forEach(button => {
  button.addEventListener('click', () => button.closest('dialog').close());
});

document.querySelectorAll('[data-remove-member]').forEach(button => {
  button.addEventListener('click', () => {
    const member = button.closest('.person-card');

    if (member.dataset.hasTasks === 'true') {
      showPopup('НЕЛЬЗЯ УДАЛИТЬ С ПРОЕКТА: ЕСТЬ ЗАДАЧИ');
      return;
    }

    member.remove();
  });
});

const notificationSettings = document.querySelector('.notification-settings');
if (notificationSettings) {
  const savedChannels = JSON.parse(localStorage.getItem('notification-channels') || '[]');
  notificationSettings.querySelectorAll('[name=channels]').forEach(checkbox => {
    checkbox.checked = savedChannels.includes(checkbox.value);
  });
}

document.querySelectorAll('form').forEach(form => {
  form.addEventListener('submit', event => {
    event.preventDefault();

    if (form.matches('.login-form')) {
      const login = form.elements.login.value.trim();
      const password = form.elements.password.value;

      if (login !== 'kozelGOAT' || password !== 'strongpassword321') {
        showPopup('Не тот логин или пароль');
        return;
      }
    }

    if (form.matches('.notification-settings')) {
      const channels = Array.from(form.querySelectorAll('[name=channels]:checked'), checkbox => checkbox.value);
      localStorage.setItem('notification-channels', JSON.stringify(channels));
      showPopup('НАСТРОЙКИ СОХРАНЕНЫ!');
      return;
    }

    const destination = form.getAttribute('action');
    if (destination) window.location.href = destination;
    else form.closest('dialog')?.close();
  });
});

