# frozen_string_literal: true

unless Object.method_defined?(:taint)
  class Object
    def taint
      self
    end

    def untaint
      self
    end

    def tainted?
      false
    end
  end
end

module Jekyll
  module Utils
    module Platforms
      def self.bash_on_windows?
        false
      end
    end
  end
end
