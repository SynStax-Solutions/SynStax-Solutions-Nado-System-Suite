import SectionHead from '../ui/SectionHead'
import './Process.css'

const DEFAULT_STEPS = [
  { num: '01', title: 'Brand identity', description: 'Naming, marks, type systems and the rules that keep them consistent past launch day.' },
  { num: '02', title: 'Packaging design', description: 'Structural and print-ready design for shelf, mailer and unboxing moments.' },
  { num: '03', title: 'Motion & animation', description: "Logo animation, social cuts and UI motion that carries the brand's energy." },
  { num: '04', title: 'Art direction', description: 'Campaign concepts, photography direction and the connective visual thread across it all.' },
]

export default function Process({
  heading = 'What we actually do.',
  description = 'No filler services. Four things, done properly.',
  steps = DEFAULT_STEPS,
}) {
  return (
    <section className="section services-section snap" id="services">
      <div className="wrap">
        <SectionHead heading={heading} description={description} />
        <div className="services-list">
          {steps.map((step) => (
            <div className="service-row" key={step.num}>
              <div className="num">{step.num}</div>
              <h3>{step.title}</h3>
              <p>{step.description}</p>
            </div>
          ))}
        </div>
      </div>
    </section>
  )
}