const list = document.getElementById("projects-list");
const template = document.getElementById("project-template");

const loading = document.getElementById("loading");
const errorMessage = document.getElementById("error-message");
const emptyState = document.getElementById("empty-state");

const addProject = document.getElementById("add-project");
const emptyAddProject = document.getElementById("empty-add-project");

const avatar = document.getElementById("avatar");
const page = document.querySelector(".page");
const description = document.getElementById("page-description");

const section = document.getElementById("projects-section");
const heading = document.getElementById("projects-heading");
const skeletonTemplate = document.getElementById("skeleton-template");

function setLoading(isLoading) {
    loading.hidden = !isLoading;
    heading.hidden = isLoading;

    page.classList.toggle("is-loading", isLoading);

    description.textContent = isLoading
        ? "Загружаем ваши проекты..."
        : "Выберите ПО для работы или создайте новый проект.";
}

function showSkeletons() {
    loading.replaceChildren();

    for (let i = 0; i < 8; i++) {
        loading.append(
            skeletonTemplate.content.cloneNode(true)
        );
    }
}

const roles = {
    Developer: "Разработчик",
    TeamLead: "Тимлид",
    Tester: "Тестировщик",
    Teacher: "Преподаватель"
};

function renderProject(project) {
    const fragment = template.content.cloneNode(true);

    fragment.querySelector(".project-name").textContent = project.name;

    fragment.querySelector(".project-role").textContent = `Вы — ${roles[project.myRole] || project.myRole}`;

    const link = fragment.querySelector(".project-link");

    link.href = `project.html?id=${encodeURIComponent(project.id)}`;

    const count = fragment.querySelector(".members-count");

    list.append(fragment);

    return count;
}

async function loadMemberCount(projectId, element) {
    try {
        const project = await getJson(`/api/projects/${encodeURIComponent(projectId)}`);
        element.textContent = project.members.length;
    } catch (error) {
        element.textContent = "—";
        element.title = error.message;
    }
}

async function loadProjects() {
    list.replaceChildren();

    list.hidden = true;
    emptyState.hidden = true;
    errorMessage.hidden = true;
    addProject.hidden = true;

    document.getElementById("account-notice").hidden = true;

    showSkeletons();
    setLoading(true);

    try {
        const [user, projects] = await Promise.all([
            getJson("/api/auth/me"),
            getJson("/api/projects")
        ]);

        avatar.textContent = user.userName
            .trim()
            .split(/\s+/)
            .slice(0, 2)
            .map(part => Array.from(part)[0])
            .join("")
            .toUpperCase();

        avatar.title = user.userName;

        document.getElementById("account-notice").hidden =
            projects.length > 0 ||
            new URLSearchParams(location.search).get("registered") !== "1";

        emptyAddProject.hidden = user.isTeacher;

        if (user.isTeacher) {
            document.getElementById("empty-description").textContent = "Проекты появятся здесь, когда вас добавят в команду.";
        }

        emptyState.hidden = projects.length > 0;
        list.hidden = projects.length === 0;

        addProject.hidden = user.isTeacher || projects.length === 0;

        const counts = projects.map(renderProject);

        setLoading(false);

        await Promise.all(
            projects.map((project, index) =>
                loadMemberCount(project.id, counts[index])
            )
        );
    } catch (error) {
        errorMessage.textContent = error.message;
        errorMessage.hidden = false;
    } finally {
        setLoading(false);
    }
}

loadProjects();