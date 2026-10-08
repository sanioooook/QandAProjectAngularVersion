import { configure } from '@testing-library/angular';

// jsdom has no layout: Material and the survey form call these.
Element.prototype.scrollIntoView ??= () => {};
window.scrollTo = () => {};

// Testing Library for Angular runs detectChanges() after every DOM event. The app is zoneless, so
// Angular already schedules change detection itself; the extra synchronous call even recurses when an
// event fires during rendering (focusing a just-added input from afterNextRender).
configure({ dom: { eventWrapper: (callback) => callback() } });
