import Reveal from '../ui/Reveal'
import './Story.css'

export default function Story() {
  return (
    <section className="nd-story" id="philosophy">
      <div className="wrap nd-story-grid">

        <div className="nd-story-copy">
          <Reveal className="nd-eyebrow">
            Our philosophy
          </Reveal>

          <Reveal
            as="h2"
            className="nd-heading nd-story-title"
            delay={80}
          >
            Where experience <em> meets </em>  imagination.

          </Reveal>
          <Reveal
            as="p"
            className="nd-lead nd-story-text"
            delay={160}
          >
            Nado Graphics began in the early 2000s with a young, fresh graphic
            designer and a passion for turning ideas into something people could
            see and remember.
            <br /><br />
            What started as a creative journey quickly became a lifelong pursuit
            of the craft. Along the way, years of hands-on experience and
            mentorship from established experts in the field shaped the way we
            think, create and solve problems.
            <br /><br />
            Every project became another lesson. Every mentor, another perspective.
            Every design, an opportunity to push the craft a little further.
            <br /><br />
            That foundation still lives at the heart of Nado Graphics today. We believe
            great design is built through curiosity, experience and the courage
            to keep evolving.
            <br /><br />
            We bring together years of industry knowledge with a fresh perspective
            - creating work that isn't simply designed to look good, but designed
            to <strong>connect, communicate and leave a lasting impression.</strong>
            <br /><br />
            <strong>Years of experience. A lifetime of curiosity. And a passion for what's next.</strong>
          </Reveal>

        </div>

        <div className="nd-story-stats">
          <Reveal className="surface-card nd-stat-card" delay={140}>
            <span className="nd-stat-value nd-grad-text-1">20+</span>
            <p>
             Years of experience turning ideas into visual identities.
            </p>
          </Reveal>

          <Reveal className="surface-card nd-stat-card" delay={220}>
            <span className="nd-stat-value nd-grad-text-2">40+</span>
            <p>
              Brands brought to life across print, digital and motion.            </p>
          </Reveal>

          <Reveal className="surface-card nd-stat-card" delay={300}>
            <span className="nd-stat-value nd-grad-text-3">1</span>
            <p>
              Passion for design that has never stopped evolving.
            </p>
          </Reveal>
        </div>

      </div>
    </section>
  )
}